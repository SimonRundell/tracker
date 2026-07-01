<?php
/**
 * PUT /assessment-defs/update.php
 *
 * Updates the part_name and/or sort_order of an existing assessment definition.
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
$id         = (int)($body['id']         ?? 0);
$part_name  = trim($body['part_name']   ?? '');
$sort_order = (int)($body['sort_order'] ?? 0);

if (!$id || !$part_name) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'id and part_name required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare(
        'UPDATE tblassessment_def SET part_name = ?, sort_order = ? WHERE id = ?'
    );
    $stmt->execute([$part_name, $sort_order, $id]);
    echo json_encode(['success' => true]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
