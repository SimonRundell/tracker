<?php
/**
 * GET /concerns/index.php
 *
 * Returns all concern categories.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

try {
    $db   = getDb();
    $stmt = $db->query('SELECT id, concern FROM tblconcern ORDER BY id');
    echo json_encode(['success' => true, 'data' => $stmt->fetchAll()]);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
