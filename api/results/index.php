<?php
/**
 * GET /results/index.php?group_id=X
 *
 * Returns all data needed to render the grade grid in a single call:
 *   - units for the group's course (with assessment_defs per unit)
 *   - students enrolled in the group with their results, raw marks,
 *     and assessment records
 *
 * Students are fetched via tblstudent_group; concern is per enrollment.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$group_id = (int)($_GET['group_id'] ?? 0);

if (!$group_id) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'group_id required']);
    exit();
}

try {
    $db = getDb();

    // Resolve course for this group
    $gStmt = $db->prepare('SELECT course_id FROM tblgroup WHERE id = ?');
    $gStmt->execute([$group_id]);
    $group = $gStmt->fetch();

    if (!$group) {
        http_response_code(404);
        echo json_encode(['success' => false, 'error' => 'Group not found']);
        exit();
    }

    $course_id = $group['course_id'];

    // Fetch units for the course
    $uStmt = $db->prepare(
        'SELECT u.id, u.unitcode, u.unitname, u.credits, u.glh, u.is_external, u.section_type, cu.year_taken
         FROM tblunit u
         JOIN tblcourseunit cu ON cu.unit_id = u.id
         WHERE cu.course_id = ?
         ORDER BY cu.year_taken, CAST(u.unitcode AS UNSIGNED), u.unitname'
    );
    $uStmt->execute([$course_id]);
    $units = $uStmt->fetchAll();

    // Fetch assessment defs for all units in this course
    $adStmt = $db->prepare(
        'SELECT ad.id, ad.unit_id, ad.part_name, ad.sort_order
         FROM tblassessment_def ad
         JOIN tblcourseunit cu ON cu.unit_id = ad.unit_id
         WHERE cu.course_id = ?
         ORDER BY ad.unit_id, ad.sort_order, ad.part_name'
    );
    $adStmt->execute([$course_id]);
    $allDefs = $adStmt->fetchAll();

    $defsByUnit = [];
    foreach ($allDefs as $def) {
        $defsByUnit[$def['unit_id']][] = [
            'id'         => (int)$def['id'],
            'part_name'  => $def['part_name'],
            'sort_order' => (int)$def['sort_order'],
        ];
    }

    foreach ($units as &$unit) {
        $unit['assessment_defs'] = $defsByUnit[$unit['id']] ?? [];
    }
    unset($unit);

    // Fetch students enrolled in this group (concern from enrollment row)
    $sStmt = $db->prepare(
        'SELECT s.id, s.firstname, s.lastname, s.cisnumber, s.notes, con.concern
         FROM   tblstudent s
         JOIN   tblstudent_group sg  ON sg.student_id = s.id AND sg.group_id = ?
         LEFT JOIN tblconcern   con  ON con.id = sg.concern_id
         ORDER BY s.lastname, s.firstname'
    );
    $sStmt->execute([$group_id]);
    $students = $sStmt->fetchAll();

    // Fetch results for all students in this group
    $rStmt = $db->prepare(
        'SELECT r.student_id, r.unit_id, r.result, r.raw_mark
         FROM tblresults r
         JOIN tblstudent_group sg ON sg.student_id = r.student_id AND sg.group_id = ?'
    );
    $rStmt->execute([$group_id]);
    $allResults = $rStmt->fetchAll();

    // Fetch assessment records for students in this group
    $aStmt = $db->prepare(
        'SELECT a.student_id, a.assessment_def_id,
                a.status, a.date_set, a.date_deadline, a.date_resubmission, a.date_completed
         FROM tblassessment a
         JOIN tblstudent_group sg ON sg.student_id = a.student_id AND sg.group_id = ?'
    );
    $aStmt->execute([$group_id]);
    $allAssessments = $aStmt->fetchAll();

    // Build results and rawMarks maps
    $resultsMap  = [];
    $rawMarksMap = [];
    foreach ($allResults as $row) {
        $resultsMap[$row['student_id']][$row['unit_id']] = $row['result'];
        if ($row['raw_mark'] !== null) {
            $rawMarksMap[$row['student_id']][$row['unit_id']] = (int)$row['raw_mark'];
        }
    }

    // Build assessments map
    $assessmentsMap = [];
    foreach ($allAssessments as $row) {
        $assessmentsMap[$row['student_id']][$row['assessment_def_id']] = [
            'status'            => $row['status'],
            'date_set'          => $row['date_set'],
            'date_deadline'     => $row['date_deadline'],
            'date_resubmission' => $row['date_resubmission'],
            'date_completed'    => $row['date_completed'],
        ];
    }

    foreach ($students as &$student) {
        $student['results']     = (object)($resultsMap[$student['id']]  ?? []);
        $student['rawMarks']    = (object)($rawMarksMap[$student['id']] ?? []);
        $student['assessments'] = (object)($assessmentsMap[$student['id']] ?? []);
    }
    unset($student);

    echo json_encode(['success' => true, 'data' => ['units' => $units, 'students' => $students]]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
