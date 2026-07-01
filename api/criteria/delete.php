<?php
/**
 * DELETE /criteria/delete.php
 *
 * Deletes a criterion. Cascades to tblevidence via FK.
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
    echo json_encode(['success' => false, 'error' => 'ID required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('DELETE FROM tblcriteria WHERE id = ?');
    $stmt->execute([$id]);

    echo json_encode([
        'success' => true,
        'message' => 'Criterion deleted. All associated student evidence has been permanently removed.',
    ]);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
