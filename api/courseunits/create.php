<?php
/**
 * POST /courseunits/create.php
 *
 * Adds a unit to a course.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$body       = json_decode(file_get_contents('php://input'), true);
$course_id  = (int)($body['course_id']  ?? 0);
$unit_id    = (int)($body['unit_id']    ?? 0);
$year_taken = (int)($body['year_taken'] ?? 1);

if (!$course_id || !$unit_id) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'course_id and unit_id required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('INSERT INTO tblcourseunit (course_id, unit_id, year_taken) VALUES (?,?,?)');
    $stmt->execute([$course_id, $unit_id, $year_taken]);

    http_response_code(201);
    echo json_encode(['success' => true, 'data' => ['id' => $db->lastInsertId()], 'message' => 'Unit added to course']);
} catch (Exception $e) {
    if ($e->getCode() == 23000) {
        http_response_code(409);
        echo json_encode(['success' => false, 'error' => 'Unit already assigned to this course']);
    } else {
        http_response_code(500);
        echo json_encode(['success' => false, 'error' => 'Server error']);
    }
}
