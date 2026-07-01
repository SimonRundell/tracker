<?php
/**
 * POST /qualtypes/reorder.php
 *
 * Accepts a JSON array of { id, sort_order } objects and bulk-updates
 * the sort_order column in tblqualificationtype.
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

$body  = json_decode(file_get_contents('php://input'), true);
$order = $body['order'] ?? [];

if (!is_array($order) || count($order) === 0) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'order array required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('UPDATE tblqualificationtype SET sort_order = ? WHERE id = ?');
    foreach ($order as $item) {
        $id         = (int)($item['id']         ?? 0);
        $sort_order = (int)($item['sort_order'] ?? 0);
        if ($id > 0) {
            $stmt->execute([$sort_order, $id]);
        }
    }
    echo json_encode(['success' => true, 'message' => 'Order saved']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
