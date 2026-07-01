<?php
/**
 * GET /reports/student-audit.php?student_id=X
 *
 * Returns the complete grade change history for a single student from
 * tblgrade_audit, ordered most-recent first.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$student_id = isset($_GET['student_id']) ? (int)$_GET['student_id'] : 0;

if (!$student_id) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'student_id required']);
    exit();
}

try {
    $db = getDb();

    // Student summary; list all enrolled courses/groups for the report header
    $sStmt = $db->prepare(
        'SELECT s.firstname, s.lastname, s.cisnumber,
                GROUP_CONCAT(DISTINCT c.coursename ORDER BY c.coursename SEPARATOR \', \') AS coursename,
                GROUP_CONCAT(DISTINCT g.groupname  ORDER BY g.groupname  SEPARATOR \', \') AS groupname
         FROM tblstudent s
         LEFT JOIN tblstudent_group sg ON sg.student_id = s.id
         LEFT JOIN tblgroup  g ON g.id  = sg.group_id
         LEFT JOIN tblcourse c ON c.id  = g.course_id
         WHERE s.id = ?
         GROUP BY s.id, s.firstname, s.lastname, s.cisnumber'
    );
    $sStmt->execute([$student_id]);
    $student = $sStmt->fetch();

    if (!$student) {
        http_response_code(404);
        echo json_encode(['success' => false, 'error' => 'Student not found']);
        exit();
    }

    // Full audit history for this student
    $aStmt = $db->prepare(
        'SELECT
             a.changed_at,
             u.unitcode,
             u.unitname,
             a.old_result,
             a.old_raw_mark,
             a.new_result,
             a.new_raw_mark,
             uu.fullname AS changed_by
         FROM tblgrade_audit a
         JOIN tblunit u  ON u.id  = a.unit_id
         JOIN tbluser uu ON uu.id = a.changed_by
         WHERE a.student_id = ?
         ORDER BY a.changed_at DESC'
    );
    $aStmt->execute([$student_id]);
    $rows = $aStmt->fetchAll();

    echo json_encode([
        'success' => true,
        'student' => $student,
        'data'    => $rows,
        'count'   => count($rows),
    ]);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
