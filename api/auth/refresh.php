<?php
/**
 * POST /auth/refresh.php
 *
 * Issues a new access token from a valid refresh token.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../config.php';
require_once __DIR__ . '/../jwt.php';

use Firebase\JWT\JWT;
use Firebase\JWT\Key;

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$body         = json_decode(file_get_contents('php://input'), true);
$refreshToken = $body['refreshToken'] ?? '';

if (!$refreshToken) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Refresh token required']);
    exit();
}

try {
    $jwtConfig = getJwtConfig();
    $decoded   = JWT::decode($refreshToken, new Key($jwtConfig['secret'], 'HS256'));

    if (($decoded->type ?? '') !== 'refresh') {
        throw new Exception('Not a refresh token');
    }

    $db   = getDb();
    $stmt = $db->prepare('SELECT id, email FROM tbluser WHERE id = ?');
    $stmt->execute([$decoded->sub]);
    $user = $stmt->fetch();

    if (!$user) {
        http_response_code(401);
        echo json_encode(['success' => false, 'error' => 'User not found']);
        exit();
    }

    $newAccess = generateAccessToken($user['id'], $user['email']);

    echo json_encode([
        'success' => true,
        'data'    => ['accessToken' => $newAccess],
    ]);
} catch (Exception $e) {
    http_response_code(401);
    echo json_encode(['success' => false, 'error' => 'Invalid or expired refresh token']);
}
