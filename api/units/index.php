<?php
/**
 * GET /units/index.php
 *
 * Returns all units.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

try {
    $db   = getDb();
    $stmt = $db->query(
        'SELECT u.id, u.unitcode, u.unitname, u.unitref, u.credits, u.glh, u.is_external, u.section_type,
                MIN(cu.year_taken) AS year_taken,
                GROUP_CONCAT(DISTINCT c.coursename ORDER BY c.coursename SEPARATOR \', \') AS courses,
                GROUP_CONCAT(DISTINCT c.id        ORDER BY c.coursename)                   AS course_ids
         FROM tblunit u
         LEFT JOIN tblcourseunit cu ON cu.unit_id = u.id
         LEFT JOIN tblcourse c      ON c.id = cu.course_id
         GROUP BY u.id
         ORDER BY MIN(cu.year_taken), u.unitname'
    );
    echo json_encode(['success' => true, 'data' => $stmt->fetchAll()]);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
