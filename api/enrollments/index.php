<?php
/**
 * GET /enrollments/index.php
 *
 * Returns enrollment records filtered by either student_id or group_id.
 * Each row includes group name, course name and concern label for display.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$studentId = isset($_GET['student_id']) ? (int)$_GET['student_id'] : null;
$groupId   = isset($_GET['group_id'])   ? (int)$_GET['group_id']   : null;

if (!$studentId && !$groupId) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'student_id or group_id required']);
    exit();
}

try {
    $db  = getDb();
    $sql = 'SELECT sg.id, sg.student_id, sg.group_id, sg.concern_id,
                   s.firstname, s.lastname, s.cisnumber,
                   g.groupname, c.id AS course_id, c.coursename,
                   con.concern
            FROM   tblstudent_group sg
            JOIN   tblstudent s  ON s.id   = sg.student_id
            JOIN   tblgroup  g   ON g.id   = sg.group_id
            JOIN   tblcourse c   ON c.id   = g.course_id
            LEFT JOIN tblconcern con ON con.id = sg.concern_id';

    if ($studentId) {
        $sql   .= ' WHERE sg.student_id = ?';
        $param  = $studentId;
    } else {
        $sql   .= ' WHERE sg.group_id = ?';
        $param  = $groupId;
    }

    $sql .= ' ORDER BY s.lastname, s.firstname';

    $stmt = $db->prepare($sql);
    $stmt->execute([$param]);
    echo json_encode(['success' => true, 'data' => $stmt->fetchAll()]);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
