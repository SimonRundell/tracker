<?php
/**
 * PUT /enrollments/update.php
 *
 * Updates the concern_id for a single enrollment row.
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
$id         = (int)($body['id'] ?? 0);
$concern_id = isset($body['concern_id']) ? ((int)$body['concern_id'] ?: null) : null;

if (!$id) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'id required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('UPDATE tblstudent_group SET concern_id = ? WHERE id = ?');
    $stmt->execute([$concern_id, $id]);

    echo json_encode(['success' => true, 'message' => 'Concern updated']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
