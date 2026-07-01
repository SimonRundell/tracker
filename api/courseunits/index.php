<?php
/**
 * GET /courseunits/index.php?course_id=X
 *
 * Returns units for a course, ordered by year_taken.
 * Includes section_type for Amendment 01 criteria support.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$course_id = (int)($_GET['course_id'] ?? 0);

if (!$course_id) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'course_id required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare(
        'SELECT u.id, u.unitcode, u.unitname, u.unitref, u.credits, u.glh, u.is_external, u.section_type, cu.year_taken
         FROM tblunit u
         JOIN tblcourseunit cu ON cu.unit_id = u.id
         WHERE cu.course_id = ?
         ORDER BY cu.year_taken, CAST(u.unitcode AS UNSIGNED), u.unitname'
    );
    $stmt->execute([$course_id]);
    echo json_encode(['success' => true, 'data' => $stmt->fetchAll()]);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
