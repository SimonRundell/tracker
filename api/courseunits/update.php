<?php
/**
 * PUT /courseunits/update.php
 *
 * Updates year_taken for a unit within a course.
 * Identified by course_id + unit_id (the composite key).
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

if ($_SERVER['REQUEST_METHOD'] !== 'PUT') {
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
    $stmt = $db->prepare('UPDATE tblcourseunit SET year_taken = ? WHERE course_id = ? AND unit_id = ?');
    $stmt->execute([$year_taken, $course_id, $unit_id]);

    echo json_encode(['success' => true, 'message' => 'Course unit updated']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
