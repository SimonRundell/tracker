<?php
/**
 * GET /reports/grade-audit.php
 *
 * Returns the grade change audit trail from tblresults.
 * Only rows where updated_by IS NOT NULL are included (i.e. a staff member
 * explicitly saved a grade).
 *
 * Optional query parameters:
 *   course_id  int  - Filter by course
 *   group_id   int  - Filter by teaching group
 *   user_id    int  - Filter by the staff member who made the change
 *   date_from  str  - ISO date (YYYY-MM-DD), start of range (inclusive)
 *   date_to    str  - ISO date (YYYY-MM-DD), end of range (inclusive)
 *
 * Returns up to 500 records, ordered most-recent first.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$courseId  = isset($_GET['course_id'])  ? (int)$_GET['course_id']  : null;
$groupId   = isset($_GET['group_id'])   ? (int)$_GET['group_id']   : null;
$userId    = isset($_GET['user_id'])    ? (int)$_GET['user_id']    : null;
$dateFrom  = isset($_GET['date_from'])  ? $_GET['date_from']        : null;
$dateTo    = isset($_GET['date_to'])    ? $_GET['date_to']          : null;

try {
    $db = getDb();

    $sql = '
        SELECT
            r.updated_at,
            s.cisnumber,
            s.firstname,
            s.lastname,
            c.coursename,
            (SELECT g2.groupname
             FROM   tblstudent_group sg2
             JOIN   tblgroup g2 ON g2.id = sg2.group_id AND g2.course_id = cu.course_id
             WHERE  sg2.student_id = s.id
             LIMIT  1) AS groupname,
            u.unitcode,
            u.unitname,
            r.result,
            r.raw_mark,
            uu.fullname AS changed_by
        FROM tblresults r
        JOIN tblstudent     s  ON s.id  = r.student_id
        JOIN tblunit        u  ON u.id  = r.unit_id
        JOIN tbluser        uu ON uu.id = r.updated_by
        JOIN tblcourseunit  cu ON cu.unit_id = u.id
        JOIN tblcourse      c  ON c.id  = cu.course_id
        WHERE r.updated_by IS NOT NULL
    ';

    $params = [];

    if ($courseId) {
        $sql     .= ' AND cu.course_id = ?';
        $params[] = $courseId;
    }
    if ($groupId) {
        $sql     .= ' AND EXISTS (SELECT 1 FROM tblstudent_group sg WHERE sg.student_id = s.id AND sg.group_id = ?)';
        $params[] = $groupId;
    }
    if ($userId) {
        $sql     .= ' AND r.updated_by = ?';
        $params[] = $userId;
    }
    if ($dateFrom) {
        $sql     .= ' AND DATE(r.updated_at) >= ?';
        $params[] = $dateFrom;
    }
    if ($dateTo) {
        $sql     .= ' AND DATE(r.updated_at) <= ?';
        $params[] = $dateTo;
    }

    $sql .= ' ORDER BY r.updated_at DESC LIMIT 500';

    $stmt = $db->prepare($sql);
    $stmt->execute($params);
    $rows = $stmt->fetchAll();

    echo json_encode(['success' => true, 'data' => $rows, 'count' => count($rows)]);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
