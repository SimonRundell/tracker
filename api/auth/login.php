<?php
/**
 * POST /auth/login.php
 *
 * Authenticates a user and returns JWT access + refresh tokens.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../config.php';
require_once __DIR__ . '/../jwt.php';

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$body = json_decode(file_get_contents('php://input'), true);
$email    = trim($body['email']    ?? '');
$password = trim($body['password'] ?? '');

if (!$email || !$password) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Email and password required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('SELECT id, email, password, fullname FROM tbluser WHERE email = ?');
    $stmt->execute([$email]);
    $user = $stmt->fetch();

    if (!$user || !password_verify($password, $user['password'])) {
        http_response_code(401);
        echo json_encode(['success' => false, 'error' => 'Invalid credentials']);
        exit();
    }

    $accessToken  = generateAccessToken($user['id'], $user['email']);
    $refreshToken = generateRefreshToken($user['id']);

    echo json_encode([
        'success' => true,
        'data'    => [
            'accessToken'  => $accessToken,
            'refreshToken' => $refreshToken,
            'user'         => [
                'id'       => $user['id'],
                'email'    => $user['email'],
                'fullname' => $user['fullname'],
            ],
        ],
    ]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
