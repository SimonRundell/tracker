<?php
/**
 * PUT /units/update.php
 *
 * Updates an existing unit. Accepts section_type for Amendment 01.
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
$unitcode    = trim($body['unitcode']     ?? '');
$unitname    = trim($body['unitname']     ?? '');
$unitref     = trim($body['unitref']      ?? '');
$credits     = (int)($body['credits']     ?? 0);
$glh         = (int)($body['glh']         ?? 0);
$is_external  = (int)($body['is_external'] ?? 0);
$year         = max(1, min(4, (int)($body['year'] ?? 1)));
$section_type = isset($body['section_type']) ? ($body['section_type'] ?: null) : false;

if (!$id || !$unitcode || !$unitname) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'id, unitcode and unitname required']);
    exit();
}

$allowedSectionTypes = [null, 'learning_objectives', 'grade_bands'];

try {
    $db = getDb();

    if ($section_type !== false) {
        if (!in_array($section_type, $allowedSectionTypes, true)) {
            http_response_code(400);
            echo json_encode(['success' => false, 'error' => 'Invalid section_type']);
            exit();
        }
        $stmt = $db->prepare('UPDATE tblunit SET unitcode=?, unitname=?, unitref=?, credits=?, glh=?, is_external=?, year=?, section_type=? WHERE id=?');
        $stmt->execute([$unitcode, $unitname, $unitref ?: null, $credits, $glh, $is_external ? 1 : 0, $year, $section_type, $id]);
    } else {
        $stmt = $db->prepare('UPDATE tblunit SET unitcode=?, unitname=?, unitref=?, credits=?, glh=?, is_external=?, year=? WHERE id=?');
        $stmt->execute([$unitcode, $unitname, $unitref ?: null, $credits, $glh, $is_external ? 1 : 0, $year, $id]);
    }

    echo json_encode(['success' => true, 'message' => 'Unit updated']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
