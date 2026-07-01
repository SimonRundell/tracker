<?php
/**
 * PUT /evidence/update.php
 *
 * Upserts a single evidence record and writes back the derived grade
 * to tblresults. The derived_grade is returned in the response.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

$decoded = requireAuth();

if ($_SERVER['REQUEST_METHOD'] !== 'PUT') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$body         = json_decode(file_get_contents('php://input'), true);
$student_id   = (int)($body['student_id']  ?? 0);
$criteria_id  = (int)($body['criteria_id'] ?? 0);
$achieved     = isset($body['achieved']) ? (int)(bool)$body['achieved'] : 0;
$achieved_date = isset($body['achieved_date']) ? trim($body['achieved_date']) : null;
$assessor     = isset($body['assessor'])     ? trim($body['assessor'])     : null;
$portfolio_ref = isset($body['portfolio_ref']) ? trim($body['portfolio_ref']) : null;
$updated_by   = (int)$decoded->sub;

if (!$student_id || !$criteria_id) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'student_id and criteria_id required']);
    exit();
}

// Validate achieved_date format if provided
if ($achieved_date && !preg_match('/^\d{4}-\d{2}-\d{2}$/', $achieved_date)) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'achieved_date must be YYYY-MM-DD format']);
    exit();
}

if ($achieved_date) {
    $d = DateTime::createFromFormat('Y-m-d', $achieved_date);
    if (!$d || $d->format('Y-m-d') !== $achieved_date) {
        http_response_code(400);
        echo json_encode(['success' => false, 'error' => 'Invalid achieved_date value']);
        exit();
    }
}

try {
    $db = getDb();

    // Verify criteria_id and get its unit_id for grade derivation
    $cStmt = $db->prepare(
        'SELECT c.id, s.unit_id
         FROM tblcriteria c
         JOIN tblunitsection s ON c.section_id = s.id
         WHERE c.id = ?'
    );
    $cStmt->execute([$criteria_id]);
    $criteriaRow = $cStmt->fetch();

    if (!$criteriaRow) {
        http_response_code(404);
        echo json_encode(['success' => false, 'error' => 'Criterion not found']);
        exit();
    }

    $unit_id = $criteriaRow['unit_id'];

    // Upsert evidence
    $eStmt = $db->prepare(
        'INSERT INTO tblevidence (student_id, criteria_id, achieved, achieved_date, assessor, portfolio_ref, updated_by)
         VALUES (?, ?, ?, ?, ?, ?, ?)
         ON DUPLICATE KEY UPDATE
           achieved      = VALUES(achieved),
           achieved_date = VALUES(achieved_date),
           assessor      = VALUES(assessor),
           portfolio_ref = VALUES(portfolio_ref),
           updated_by    = VALUES(updated_by)'
    );
    $eStmt->execute([
        $student_id,
        $criteria_id,
        $achieved,
        $achieved_date ?: null,
        $assessor     ?: null,
        $portfolio_ref ?: null,
        $updated_by,
    ]);

    // Derive grade from all evidence for this student+unit
    $unitStmt = $db->prepare('SELECT section_type FROM tblunit WHERE id = ?');
    $unitStmt->execute([$unit_id]);
    $unit = $unitStmt->fetch();

    $derived_grade = 'NS';

    if ($unit && $unit['section_type']) {
        // Get all sections + criteria
        $sStmt = $db->prepare(
            'SELECT s.id AS section_id, s.label AS section_label, c.id AS criteria_id
             FROM tblunitsection s
             JOIN tblcriteria c ON c.section_id = s.id
             WHERE s.unit_id = ?
             ORDER BY s.sort_order, c.sort_order'
        );
        $sStmt->execute([$unit_id]);
        $rows = $sStmt->fetchAll();

        // Get all evidence for this student+unit
        $evStmt = $db->prepare(
            'SELECT e.criteria_id, e.achieved
             FROM tblevidence e
             JOIN tblcriteria c ON c.id = e.criteria_id
             JOIN tblunitsection s ON s.id = c.section_id
             WHERE e.student_id = ? AND s.unit_id = ?'
        );
        $evStmt->execute([$student_id, $unit_id]);
        $evRows = $evStmt->fetchAll();

        $evidenceMap = [];
        foreach ($evRows as $ev) {
            $evidenceMap[$ev['criteria_id']] = (bool)$ev['achieved'];
        }

        // Group criteria by section_label
        $bySection = [];
        foreach ($rows as $r) {
            $bySection[$r['section_label']][] = $r['criteria_id'];
        }

        if ($unit['section_type'] === 'grade_bands') {
            $bandOrder = ['P', 'M', 'D'];
            $highest   = 'NS';
            foreach ($bandOrder as $band) {
                if (!isset($bySection[$band]) || empty($bySection[$band])) continue;
                $allMet = true;
                foreach ($bySection[$band] as $cid) {
                    if (!($evidenceMap[$cid] ?? false)) { $allMet = false; break; }
                }
                if (!$allMet) break;
                $highest = $band;
            }
            $derived_grade = $highest;
        } elseif ($unit['section_type'] === 'learning_objectives') {
            $totalCount   = count($rows);
            $achievedCount = 0;
            foreach ($rows as $r) {
                if ($evidenceMap[$r['criteria_id']] ?? false) $achievedCount++;
            }
            if ($totalCount > 0 && $achievedCount === $totalCount) {
                $derived_grade = 'P';
            } elseif ($achievedCount > 0) {
                $derived_grade = 'U';
            } else {
                $derived_grade = 'NS';
            }
        }

        // Write derived grade back to tblresults
        $rStmt = $db->prepare(
            'INSERT INTO tblresults (student_id, unit_id, result, updated_by)
             VALUES (?, ?, ?, ?)
             ON DUPLICATE KEY UPDATE result = VALUES(result), updated_by = VALUES(updated_by)'
        );
        $rStmt->execute([$student_id, $unit_id, $derived_grade, $updated_by]);
    }

    echo json_encode([
        'success' => true,
        'data'    => ['derived_grade' => $derived_grade],
        'message' => 'Evidence saved. Derived grade: ' . $derived_grade,
    ]);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
