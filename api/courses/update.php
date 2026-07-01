<?php
/**
 * PUT /courses/update.php
 *
 * Updates an existing course. The qual_type is validated against tblqualificationtype.
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

$body               = json_decode(file_get_contents('php://input'), true);
$id                 = (int)($body['id']                 ?? 0);
$coursename         = trim($body['coursename']          ?? '');
$total_credits      = (int)($body['total_credits']      ?? 0);
$pass_points        = (int)($body['pass_points']        ?? 0);
$merit_points       = (int)($body['merit_points']       ?? 0);
$distinction_points = (int)($body['distinction_points'] ?? 0);
$raw_qual_type      = trim($body['qual_type']           ?? '');

if (!$id || !$coursename) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'ID and course name required']);
    exit();
}

try {
    $db = getDb();

    // Validate qual_type against the qualification type table
    $chk = $db->prepare('SELECT code FROM tblqualificationtype WHERE code = ?');
    $chk->execute([$raw_qual_type]);
    $qual_type = $chk->fetchColumn() ?: 'other';

    $stmt = $db->prepare(
        'UPDATE tblcourse
         SET coursename=?, total_credits=?, pass_points=?, merit_points=?,
             distinction_points=?, qual_type=?
         WHERE id=?'
    );
    $stmt->execute([$coursename, $total_credits, $pass_points, $merit_points, $distinction_points, $qual_type, $id]);

    echo json_encode(['success' => true, 'message' => 'Course updated']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
