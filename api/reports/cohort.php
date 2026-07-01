<?php
/**
 * GET /reports/cohort.php?course_id=X[&group_id=Y]
 *
 * Returns all units and students (with results) for a course, optionally
 * filtered to a single teaching group. Used by the At-Risk, Grade
 * Distribution and Unit Performance reports.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$course_id = isset($_GET['course_id']) ? (int)$_GET['course_id'] : 0;
$group_id  = isset($_GET['group_id'])  ? (int)$_GET['group_id']  : null;

if (!$course_id) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'course_id required']);
    exit();
}

try {
    $db = getDb();

    // Course info
    $cStmt = $db->prepare('SELECT id, coursename, qual_type, pass_points, merit_points, distinction_points, total_credits FROM tblcourse WHERE id = ?');
    $cStmt->execute([$course_id]);
    $course = $cStmt->fetch();

    if (!$course) {
        http_response_code(404);
        echo json_encode(['success' => false, 'error' => 'Course not found']);
        exit();
    }

    // Units for the course
    $uStmt = $db->prepare(
        'SELECT u.id, u.unitcode, u.unitname, u.credits, u.glh, u.is_external, u.section_type, cu.year_taken
         FROM tblunit u
         JOIN tblcourseunit cu ON cu.unit_id = u.id
         WHERE cu.course_id = ?
         ORDER BY cu.year_taken, CAST(u.unitcode AS UNSIGNED), u.unitname'
    );
    $uStmt->execute([$course_id]);
    $units = $uStmt->fetchAll();

    // Students — whole course or single group (concern is per enrollment)
    $sql    = 'SELECT s.id, s.firstname, s.lastname, s.cisnumber, g.groupname, co.concern
               FROM   tblstudent s
               JOIN   tblstudent_group sg ON sg.student_id = s.id
               JOIN   tblgroup   g        ON g.id  = sg.group_id AND g.course_id = ?
               LEFT JOIN tblconcern co    ON co.id = sg.concern_id';
    $params = [$course_id];
    if ($group_id) {
        $sql     .= ' AND sg.group_id = ?';
        $params[] = $group_id;
    }
    $sql .= ' ORDER BY s.lastname, s.firstname';

    $sStmt = $db->prepare($sql);
    $sStmt->execute($params);
    $students = $sStmt->fetchAll();

    // Results for all matched students
    $studentIds = array_column($students, 'id');
    $resultsMap = [];
    $rawMarksMap = [];

    if ($studentIds) {
        $placeholders = implode(',', array_fill(0, count($studentIds), '?'));
        $rStmt = $db->prepare(
            "SELECT r.student_id, r.unit_id, r.result, r.raw_mark
             FROM tblresults r
             WHERE r.student_id IN ($placeholders)"
        );
        $rStmt->execute($studentIds);
        foreach ($rStmt->fetchAll() as $row) {
            $resultsMap[$row['student_id']][$row['unit_id']] = $row['result'];
            if ($row['raw_mark'] !== null) {
                $rawMarksMap[$row['student_id']][$row['unit_id']] = (int)$row['raw_mark'];
            }
        }
    }

    foreach ($students as &$student) {
        $student['results']  = (object)($resultsMap[$student['id']]  ?? []);
        $student['rawMarks'] = (object)($rawMarksMap[$student['id']] ?? []);
    }

    echo json_encode([
        'success'  => true,
        'course'   => $course,
        'units'    => $units,
        'students' => $students,
    ]);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
