<?php
/**
 * PUT /users/update.php
 *
 * Updates a user's email and fullname only.
 * Password changes are handled by auth/change-password.php.
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

$body     = json_decode(file_get_contents('php://input'), true);
$id       = (int)($body['id']       ?? 0);
$email    = trim($body['email']    ?? '');
$fullname = trim($body['fullname'] ?? '');

if (!$id || !$email || !$fullname) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'id, email and fullname required']);
    exit();
}

if (!filter_var($email, FILTER_VALIDATE_EMAIL)) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Invalid email address']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('UPDATE tbluser SET email=?, fullname=? WHERE id=?');
    $stmt->execute([$email, $fullname, $id]);

    echo json_encode(['success' => true, 'message' => 'User updated']);
} catch (Exception $e) {
    if ($e->getCode() == 23000) {
        http_response_code(409);
        echo json_encode(['success' => false, 'error' => 'Email already in use']);
    } else {
        http_response_code(500);
        echo json_encode(['success' => false, 'error' => 'Server error']);
    }
}
