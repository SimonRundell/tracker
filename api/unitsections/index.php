<?php
/**
 * GET /unitsections/index.php?unit_id=X
 *
 * Returns all sections for a unit, ordered by sort_order.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$unit_id = (int)($_GET['unit_id'] ?? 0);

if (!$unit_id) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'unit_id required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('SELECT id, unit_id, label, title, sort_order FROM tblunitsection WHERE unit_id = ? ORDER BY sort_order, label');
    $stmt->execute([$unit_id]);
    echo json_encode(['success' => true, 'data' => $stmt->fetchAll()]);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
