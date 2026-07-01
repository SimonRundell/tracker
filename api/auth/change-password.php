<?php
/**
 * POST /auth/change-password.php
 *
 * Changes the authenticated user's password.
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

$decoded = requireAuth();

$body            = json_decode(file_get_contents('php://input'), true);
$currentPassword = $body['currentPassword'] ?? '';
$newPassword     = $body['newPassword']     ?? '';

if (!$currentPassword || !$newPassword) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Both current and new password required']);
    exit();
}

if (strlen($newPassword) < 8) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'New password must be at least 8 characters']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('SELECT id, password FROM tbluser WHERE id = ?');
    $stmt->execute([$decoded->sub]);
    $user = $stmt->fetch();

    if (!$user || !password_verify($currentPassword, $user['password'])) {
        http_response_code(401);
        echo json_encode(['success' => false, 'error' => 'Current password incorrect']);
        exit();
    }

    $hash   = password_hash($newPassword, PASSWORD_BCRYPT, ['cost' => 12]);
    $update = $db->prepare('UPDATE tbluser SET password = ? WHERE id = ?');
    $update->execute([$hash, $user['id']]);

    echo json_encode(['success' => true, 'message' => 'Password updated successfully']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
