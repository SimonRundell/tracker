<?php
/**
 * POST /qualtypes/create.php
 *
 * Creates a new qualification type. The code field must be unique and is
 * immutable after creation.
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
$code        = trim($body['code']         ?? '');
$display     = trim($body['display_name'] ?? '');
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

if (!$code || !$display) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Code and display name are required']);
    exit();
}

// Sanitise code: lowercase, alphanumeric + underscore only
if (!preg_match('/^[a-z0-9_]+$/', $code)) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Code must contain only lowercase letters, digits, and underscores']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare(
        'INSERT INTO tblqualificationtype
         (code, display_name, default_total_credits, default_pass_points,
          default_merit_points, default_distinction_points, max_years,
          is_btec, is_ncfe, btec_overall_grades, show_predict, sort_order)
         VALUES (?,?,?,?,?,?,?,?,?,?,?,?)'
    );
    $stmt->execute([
        $code, $display, $def_credits, $def_pass,
        $def_merit, $def_dist, $max_years,
        $is_btec, $is_ncfe, $btec_ov, $show_pred, $sort_order,
    ]);
    $id = $db->lastInsertId();

    http_response_code(201);
    echo json_encode(['success' => true, 'data' => ['id' => $id], 'message' => 'Qualification type created']);
} catch (PDOException $e) {
    if ($e->getCode() === '23000') {
        http_response_code(409);
        echo json_encode(['success' => false, 'error' => "Code '$code' is already in use"]);
    } else {
        http_response_code(500);
        echo json_encode(['success' => false, 'error' => 'Server error']);
    }
}
