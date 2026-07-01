<?php
/**
 * GET /criteria/index.php
 *
 * Two modes:
 *   ?unit_id=X    Returns all criteria grouped by section (for ObjectivesModal)
 *   ?section_id=X Returns criteria for a single section (for ObjectivesManager)
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$unit_id    = isset($_GET['unit_id'])    ? (int)$_GET['unit_id']    : null;
$section_id = isset($_GET['section_id']) ? (int)$_GET['section_id'] : null;

if (!$unit_id && !$section_id) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'unit_id or section_id required']);
    exit();
}

try {
    $db = getDb();

    if ($unit_id) {
        // Fetch sections
        $sStmt = $db->prepare('SELECT id, label, title, sort_order FROM tblunitsection WHERE unit_id = ? ORDER BY sort_order, label');
        $sStmt->execute([$unit_id]);
        $sections = $sStmt->fetchAll();

        // Fetch all criteria for the unit
        $cStmt = $db->prepare(
            'SELECT c.id, c.section_id, c.code, c.description, c.sort_order
             FROM tblcriteria c
             JOIN tblunitsection s ON c.section_id = s.id
             WHERE s.unit_id = ?
             ORDER BY s.sort_order, c.sort_order'
        );
        $cStmt->execute([$unit_id]);
        $allCriteria = $cStmt->fetchAll();

        // Group criteria by section
        $criteriaBySection = [];
        foreach ($allCriteria as $c) {
            $criteriaBySection[$c['section_id']][] = $c;
        }

        $result = [];
        foreach ($sections as $s) {
            $result[] = [
                'section_id'    => $s['id'],
                'section_label' => $s['label'],
                'section_title' => $s['title'],
                'sort_order'    => $s['sort_order'],
                'criteria'      => $criteriaBySection[$s['id']] ?? [],
            ];
        }

        echo json_encode(['success' => true, 'data' => $result]);
    } else {
        // Single section mode
        $stmt = $db->prepare('SELECT id, section_id, code, description, sort_order FROM tblcriteria WHERE section_id = ? ORDER BY sort_order, code');
        $stmt->execute([$section_id]);
        echo json_encode(['success' => true, 'data' => $stmt->fetchAll()]);
    }
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
