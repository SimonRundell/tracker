<?php
/**
 * GET /assessment-defs/index.php?unit_id=X
 *
 * Returns all assessment part definitions for a given unit,
 * ordered by sort_order then part_name.
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
    $stmt = $db->prepare(
        'SELECT id, unit_id, part_name, sort_order
         FROM tblassessment_def
         WHERE unit_id = ?
         ORDER BY sort_order, part_name'
    );
    $stmt->execute([$unit_id]);
    echo json_encode(['success' => true, 'data' => $stmt->fetchAll()]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
