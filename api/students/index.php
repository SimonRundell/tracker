<?php
/**
 * GET /students/index.php
 *
 * Returns all student records. Each row includes a comma-separated
 * list of courses the student is currently enrolled on (for display
 * and filtering in the admin UI).
 *
 * Optional filter: ?group_id=X returns only students enrolled in that group.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

try {
    $db      = getDb();
    $groupId = isset($_GET['group_id']) ? (int)$_GET['group_id'] : null;

    $sql = 'SELECT s.id, s.firstname, s.lastname, s.cisnumber,
                   GROUP_CONCAT(DISTINCT c.coursename ORDER BY c.coursename SEPARATOR \', \') AS courses
            FROM   tblstudent s
            LEFT JOIN tblstudent_group sg ON sg.student_id = s.id
            LEFT JOIN tblgroup  g  ON g.id  = sg.group_id
            LEFT JOIN tblcourse c  ON c.id  = g.course_id';

    $params = [];
    if ($groupId) {
        $sql     .= ' WHERE sg.group_id = ?';
        $params[] = $groupId;
    }

    $sql .= ' GROUP BY s.id, s.firstname, s.lastname, s.cisnumber
              ORDER BY s.lastname, s.firstname';

    $stmt = $db->prepare($sql);
    $stmt->execute($params);
    echo json_encode(['success' => true, 'data' => $stmt->fetchAll()]);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
