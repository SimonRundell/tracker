<?php
/**
 * POST /unitsections/create.php
 *
 * Creates a new unit section (LO or grade band).
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

$body       = json_decode(file_get_contents('php://input'), true);
$unit_id    = (int)($body['unit_id']    ?? 0);
$label      = trim($body['label']      ?? '');
$title      = trim($body['title']      ?? '') ?: null;
$sort_order = (int)($body['sort_order'] ?? 0);

if (!$unit_id || !$label) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'unit_id and label required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('INSERT INTO tblunitsection (unit_id, label, title, sort_order) VALUES (?,?,?,?)');
    $stmt->execute([$unit_id, $label, $title, $sort_order]);

    http_response_code(201);
    echo json_encode(['success' => true, 'data' => ['id' => $db->lastInsertId()], 'message' => 'Section created']);
} catch (Exception $e) {
    if ($e->getCode() == 23000) {
        http_response_code(409);
        echo json_encode(['success' => false, 'error' => 'Section label already exists for this unit']);
    } else {
        http_response_code(500);
        echo json_encode(['success' => false, 'error' => 'Server error']);
    }
}
