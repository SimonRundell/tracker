<?php
/**
 * POST /units/create.php
 *
 * Creates a new unit.
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

$body        = json_decode(file_get_contents('php://input'), true);
$unitcode    = trim($body['unitcode']    ?? '');
$unitname    = trim($body['unitname']    ?? '');
$unitref     = trim($body['unitref']     ?? '');
$credits     = (int)($body['credits']    ?? 0);
$glh         = (int)($body['glh']        ?? 0);
$is_external = (int)($body['is_external'] ?? 0);
$year        = max(1, min(4, (int)($body['year'] ?? 1)));

if (!$unitcode || !$unitname) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'unitcode and unitname required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('INSERT INTO tblunit (unitcode, unitname, unitref, credits, glh, is_external, year) VALUES (?,?,?,?,?,?,?)');
    $stmt->execute([$unitcode, $unitname, $unitref ?: null, $credits, $glh, $is_external ? 1 : 0, $year]);

    http_response_code(201);
    echo json_encode(['success' => true, 'data' => ['id' => $db->lastInsertId()], 'message' => 'Unit created']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
