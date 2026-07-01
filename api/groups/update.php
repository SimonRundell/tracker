<?php
/**
 * PUT /groups/update.php
 *
 * Updates an existing group.
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

$body      = json_decode(file_get_contents('php://input'), true);
$id        = (int)($body['id']        ?? 0);
$course_id = (int)($body['course_id'] ?? 0);
$groupname = trim($body['groupname']  ?? '');

if (!$id || !$course_id || !$groupname) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'id, course_id and groupname required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('UPDATE tblgroup SET course_id=?, groupname=? WHERE id=?');
    $stmt->execute([$course_id, $groupname, $id]);

    echo json_encode(['success' => true, 'message' => 'Group updated']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
