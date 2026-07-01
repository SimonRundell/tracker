<?php
/**
 * PUT /qualtypes/update.php
 *
 * Updates a qualification type. The code field is read-only and is ignored
 * even if supplied.
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
$id          = (int)($body['id']            ?? 0);
$display     = trim($body['display_name']   ?? '');
$def_credits = (int)($body['default_total_credits']      ?? 0);
$def_pass    = (int)($body['default_pass_points']        ?? 0);
$def_merit   = (int)($body['default_merit_points']       ?? 0);
$def_dist    = (int)($body['default_distinction_points'] ?? 0);
$max_years   = max(1, (int)($body['max_years']   ?? 2));
$is_btec     = (int)!empty($body['is_btec']);
$is_ncfe     = (int)!empty($body['is_ncfe']);
$btec_ov     = (int)!empty($body['btec_overall_grades']);
$show_pred   = (int)!empty($body['show_predict']);
$sort_order  = (int)($body['sort_order'] ?? 0);

if (!$id || !$display) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'ID and display name are required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare(
        'UPDATE tblqualificationtype
         SET display_name=?, default_total_credits=?, default_pass_points=?,
             default_merit_points=?, default_distinction_points=?,
             max_years=?, is_btec=?, is_ncfe=?, btec_overall_grades=?,
             show_predict=?, sort_order=?
         WHERE id=?'
    );
    $stmt->execute([
        $display, $def_credits, $def_pass,
        $def_merit, $def_dist, $max_years,
        $is_btec, $is_ncfe, $btec_ov, $show_pred, $sort_order, $id,
    ]);

    echo json_encode(['success' => true, 'message' => 'Qualification type updated']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
