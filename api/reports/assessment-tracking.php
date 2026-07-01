<?php
/**
 * GET /reports/assessment-tracking.php
 *
 * Returns flat assessment-tracking rows for the four assessment reports.
 * Filters by course and optionally by teaching group and/or individual student.
 * When student_id is supplied the response also includes all course units and
 * the student's unit grades from tblresults.
 *
 * Required: course_id  (may be omitted when student_id is provided alone —
 *           the course will be resolved from the student record)
 * Optional: group_id, student_id
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$course_id  = isset($_GET['course_id'])  ? (int)$_GET['course_id']  : 0;
$group_id   = isset($_GET['group_id'])   ? (int)$_GET['group_id']   : null;
$student_id = isset($_GET['student_id']) ? (int)$_GET['student_id'] : null;

if (!$course_id && !$student_id) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'course_id or student_id required']);
    exit();
}

try {
    $db = getDb();

    // Resolve course_id from the student's first enrollment when not supplied directly
    if (!$course_id && $student_id) {
        $stRow = $db->prepare(
            'SELECT g.course_id FROM tblstudent_group sg
             JOIN tblgroup g ON g.id = sg.group_id
             WHERE sg.student_id = ? LIMIT 1'
        );
        $stRow->execute([$student_id]);
        $r = $stRow->fetch();
        if (!$r) {
            http_response_code(404);
            echo json_encode(['success' => false, 'error' => 'Student not found or has no enrollment']);
            exit();
        }
        $course_id = (int)$r['course_id'];
    }

    // Course details
    $cStmt = $db->prepare('SELECT id, coursename, qual_type FROM tblcourse WHERE id = ?');
    $cStmt->execute([$course_id]);
    $course = $cStmt->fetch();

    if (!$course) {
        http_response_code(404);
        echo json_encode(['success' => false, 'error' => 'Course not found']);
        exit();
    }

    // Main flat query joining students -> units -> assessment definitions -> tracking rows
    $sql = '
        SELECT
            s.id             AS student_id,
            s.firstname,
            s.lastname,
            s.cisnumber,
            g.groupname,
            u.id             AS unit_id,
            u.unitcode,
            u.unitname,
            cu.year_taken,
            ad.id            AS assessment_def_id,
            ad.part_name,
            ad.sort_order    AS part_order,
            COALESCE(a.status, \'NOT_SET\') AS status,
            a.date_set,
            a.date_deadline,
            a.date_resubmission,
            a.date_completed
        FROM tblstudent s
        JOIN tblstudent_group sg   ON sg.student_id = s.id
        JOIN tblgroup g            ON g.id = sg.group_id AND g.course_id = ?
        JOIN tblcourseunit cu      ON cu.course_id = g.course_id
        JOIN tblunit u             ON u.id  = cu.unit_id
        JOIN tblassessment_def ad  ON ad.unit_id = u.id
        LEFT JOIN tblassessment a  ON a.student_id = s.id
                                  AND a.assessment_def_id = ad.id';

    $params = [$course_id];

    if ($group_id) {
        $sql     .= ' AND sg.group_id = ?';
        $params[] = $group_id;
    }
    if ($student_id) {
        $sql     .= ' AND s.id = ?';
        $params[] = $student_id;
    }

    $sql .= ' ORDER BY s.lastname, s.firstname,
                       cu.year_taken, CAST(u.unitcode AS UNSIGNED), u.unitname,
                       ad.sort_order';

    $stmt = $db->prepare($sql);
    $stmt->execute($params);
    $rows = $stmt->fetchAll();

    $response = ['success' => true, 'course' => $course, 'rows' => $rows];

    // When reporting on a single student, include all course units and their grades
    if ($student_id) {
        $uStmt = $db->prepare(
            'SELECT u.id, u.unitcode, u.unitname, cu.year_taken
             FROM tblunit u
             JOIN tblcourseunit cu ON cu.unit_id = u.id
             WHERE cu.course_id = ?
             ORDER BY cu.year_taken, CAST(u.unitcode AS UNSIGNED), u.unitname'
        );
        $uStmt->execute([$course_id]);
        $response['units'] = $uStmt->fetchAll();

        $gStmt = $db->prepare(
            'SELECT r.unit_id, r.result, r.raw_mark
             FROM tblresults r
             JOIN tblcourseunit cu ON cu.unit_id = r.unit_id AND cu.course_id = ?
             WHERE r.student_id = ?'
        );
        $gStmt->execute([$course_id, $student_id]);
        $gradesMap = [];
        foreach ($gStmt->fetchAll() as $g) {
            $gradesMap[$g['unit_id']] = ['result' => $g['result'], 'raw_mark' => $g['raw_mark']];
        }
        $response['grades'] = $gradesMap;
    }

    echo json_encode($response);

} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
