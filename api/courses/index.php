<?php
/**
 * GET /courses/index.php
 *
 * Returns all courses.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

try {
    $db   = getDb();
    $stmt = $db->query('SELECT id, coursename, total_credits, pass_points, merit_points, distinction_points, qual_type FROM tblcourse ORDER BY coursename');
    echo json_encode(['success' => true, 'data' => $stmt->fetchAll()]);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
