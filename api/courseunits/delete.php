<?php
/**
 * DELETE /courseunits/delete.php
 *
 * Removes a unit from a course.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

if ($_SERVER['REQUEST_METHOD'] !== 'DELETE') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$body      = json_decode(file_get_contents('php://input'), true);
$course_id = (int)($body['course_id'] ?? 0);
$unit_id   = (int)($body['unit_id']   ?? 0);

if (!$course_id || !$unit_id) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'course_id and unit_id required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('DELETE FROM tblcourseunit WHERE course_id = ? AND unit_id = ?');
    $stmt->execute([$course_id, $unit_id]);

    echo json_encode(['success' => true, 'message' => 'Unit removed from course']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
