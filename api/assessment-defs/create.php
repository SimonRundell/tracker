<?php
/**
 * POST /assessment-defs/create.php
 *
 * Creates a new assessment part definition for a unit.
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

$body      = json_decode(file_get_contents('php://input'), true);
$unit_id   = (int)($body['unit_id']   ?? 0);
$part_name = trim($body['part_name']  ?? '');
$sort_order = (int)($body['sort_order'] ?? 0);

if (!$unit_id || !$part_name) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'unit_id and part_name required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare(
        'INSERT INTO tblassessment_def (unit_id, part_name, sort_order) VALUES (?, ?, ?)'
    );
    $stmt->execute([$unit_id, $part_name, $sort_order]);
    $newId = (int)$db->lastInsertId();
    echo json_encode(['success' => true, 'data' => ['id' => $newId]]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
