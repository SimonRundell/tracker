<?php
/**
 * PUT /criteria/update.php
 *
 * Updates an existing criterion.
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

$body        = json_decode(file_get_contents('php://input'), true);
$id          = (int)($body['id']          ?? 0);
$code        = trim($body['code']         ?? '');
$description = isset($body['description']) ? (trim($body['description']) ?: null) : null;
$sort_order  = (int)($body['sort_order']  ?? 0);

if (!$id || !$code) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'id and code required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('UPDATE tblcriteria SET code=?, description=?, sort_order=? WHERE id=?');
    $stmt->execute([$code, $description, $sort_order, $id]);

    echo json_encode(['success' => true, 'message' => 'Criterion updated']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
