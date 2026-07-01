<?php
/**
 * PUT /concerns/update.php
 *
 * Updates a concern category.
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

$body    = json_decode(file_get_contents('php://input'), true);
$id      = (int)($body['id']      ?? 0);
$concern = trim($body['concern'] ?? '');

if (!$id || !$concern) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'id and concern required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('UPDATE tblconcern SET concern=? WHERE id=?');
    $stmt->execute([$concern, $id]);

    echo json_encode(['success' => true, 'message' => 'Concern updated']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
