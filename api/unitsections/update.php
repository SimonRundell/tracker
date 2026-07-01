<?php
/**
 * PUT /unitsections/update.php
 *
 * Updates an existing unit section.
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
$label      = trim($body['label']      ?? '');
$title      = isset($body['title']) ? (trim($body['title']) ?: null) : false;
$sort_order = isset($body['sort_order']) ? (int)$body['sort_order'] : false;

if (!$id || !$label) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'id and label required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('UPDATE tblunitsection SET label=?, title=?, sort_order=? WHERE id=?');
    $stmt->execute([
        $label,
        $title !== false ? $title : null,
        $sort_order !== false ? $sort_order : 0,
        $id,
    ]);

    echo json_encode(['success' => true, 'message' => 'Section updated']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
