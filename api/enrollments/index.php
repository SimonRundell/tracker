<?php
/**
 * GET /enrollments/index.php
 *
 * Returns enrollment records. Accepts one of:
 *   ?student_id=X   — all groups for one student
 *   ?group_id=X     — all students in one group
 *   ?all=1          — every enrollment row (admin use)
 *
 * Each row includes student name, group name, course name, and concern label.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$studentId = isset($_GET['student_id']) ? (int)$_GET['student_id'] : null;
$groupId   = isset($_GET['group_id'])   ? (int)$_GET['group_id']   : null;
$all       = isset($_GET['all'])        && $_GET['all'] === '1';

if (!$studentId && !$groupId && !$all) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'student_id, group_id, or all=1 required']);
    exit();
}

try {
    $db  = getDb();
    $sql = 'SELECT sg.id, sg.student_id, sg.group_id, sg.concern_id,
                   s.firstname, s.lastname, s.cisnumber,
                   g.groupname, c.id AS course_id, c.coursename,
                   con.concern
            FROM   tblstudent_group sg
            JOIN   tblstudent    s   ON s.id  = sg.student_id
            JOIN   tblgroup      g   ON g.id  = sg.group_id
            JOIN   tblcourse     c   ON c.id  = g.course_id
            LEFT JOIN tblconcern con ON con.id = sg.concern_id';

    $conditions = [];
    $params     = [];
    if ($studentId) { $conditions[] = 'sg.student_id = ?'; $params[] = $studentId; }
    if ($groupId)   { $conditions[] = 'sg.group_id = ?';   $params[] = $groupId; }
    if ($conditions) $sql .= ' WHERE ' . implode(' AND ', $conditions);
    $sql .= ' ORDER BY s.lastname, s.firstname, c.coursename, g.groupname';

    $stmt = $db->prepare($sql);
    $stmt->execute($params);
    echo json_encode(['success' => true, 'data' => $stmt->fetchAll()]);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
