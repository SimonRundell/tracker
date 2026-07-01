<?php
/**
 * GET /qualtypes/index.php
 *
 * Returns all qualification type definitions, ordered by sort_order then display_name.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

try {
    $db   = getDb();
    $stmt = $db->query(
        'SELECT id, code, display_name,
                default_total_credits, default_pass_points,
                default_merit_points, default_distinction_points,
                max_years, is_btec, is_ncfe, btec_overall_grades,
                show_predict, sort_order
         FROM tblqualificationtype
         ORDER BY sort_order, display_name'
    );
    echo json_encode(['success' => true, 'data' => $stmt->fetchAll()]);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
