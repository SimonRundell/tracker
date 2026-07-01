<?php
/**
 * DELETE /enrollments/delete.php
 *
 * Removes a student from a teaching group.
 * The student record and all their results are retained.
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

$body = json_decode(file_get_contents('php://input'), true);
$id   = (int)($body['id'] ?? 0);

if (!$id) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'id required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('DELETE FROM tblstudent_group WHERE id = ?');
    $stmt->execute([$id]);

    echo json_encode(['success' => true, 'message' => 'Enrollment removed']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
