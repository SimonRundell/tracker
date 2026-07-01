<?php
/**
 * GET /evidence/index.php?student_id=X&unit_id=Y
 *
 * Returns all evidence rows for a student across all criteria in a unit.
 * Uses LEFT JOIN + COALESCE so unrecorded criteria appear with achieved=false.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$student_id = (int)($_GET['student_id'] ?? 0);
$unit_id    = (int)($_GET['unit_id']    ?? 0);

if (!$student_id || !$unit_id) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'student_id and unit_id required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare(
        'SELECT
           c.id           AS criteria_id,
           c.code         AS criteria_code,
           COALESCE(e.achieved, 0)  AS achieved,
           e.achieved_date,
           e.assessor,
           e.portfolio_ref
         FROM tblcriteria c
         JOIN tblunitsection s ON c.section_id = s.id
         LEFT JOIN tblevidence e
           ON e.criteria_id = c.id AND e.student_id = ?
         WHERE s.unit_id = ?
         ORDER BY s.sort_order, c.sort_order'
    );
    $stmt->execute([$student_id, $unit_id]);
    $rows = $stmt->fetchAll();

    // Cast achieved to bool
    foreach ($rows as &$row) {
        $row['achieved'] = (bool)$row['achieved'];
    }

    echo json_encode(['success' => true, 'data' => $rows]);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
