<?php
/**
 * POST /groups/create.php
 *
 * Creates a new teaching group.
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

$body      = json_decode(file_get_contents('php://input'), true);
$course_id = (int)($body['course_id'] ?? 0);
$groupname = trim($body['groupname']  ?? '');

if (!$course_id || !$groupname) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'course_id and groupname required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('INSERT INTO tblgroup (course_id, groupname) VALUES (?,?)');
    $stmt->execute([$course_id, $groupname]);

    http_response_code(201);
    echo json_encode(['success' => true, 'data' => ['id' => $db->lastInsertId()], 'message' => 'Group created']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
