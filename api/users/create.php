<?php
/**
 * POST /users/create.php
 *
 * Creates a new system user. Hashes password with bcrypt cost 12.
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

$body     = json_decode(file_get_contents('php://input'), true);
$email    = trim($body['email']    ?? '');
$fullname = trim($body['fullname'] ?? '');
$password = $body['password']      ?? '';

if (!$email || !$fullname || !$password) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'email, fullname and password required']);
    exit();
}

if (!filter_var($email, FILTER_VALIDATE_EMAIL)) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Invalid email address']);
    exit();
}

if (strlen($password) < 8) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Password must be at least 8 characters']);
    exit();
}

try {
    $db   = getDb();
    $hash = password_hash($password, PASSWORD_BCRYPT, ['cost' => 12]);
    $stmt = $db->prepare('INSERT INTO tbluser (email, password, fullname) VALUES (?,?,?)');
    $stmt->execute([$email, $hash, $fullname]);

    http_response_code(201);
    echo json_encode(['success' => true, 'data' => ['id' => $db->lastInsertId()], 'message' => 'User created']);
} catch (Exception $e) {
    if ($e->getCode() == 23000) {
        http_response_code(409);
        echo json_encode(['success' => false, 'error' => 'Email already in use']);
    } else {
        http_response_code(500);
        echo json_encode(['success' => false, 'error' => 'Server error']);
    }
}
