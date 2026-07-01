<?php
/**
 * POST /courses/create.php
 *
 * Creates a new course. The qual_type is validated against tblqualificationtype.
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

$body               = json_decode(file_get_contents('php://input'), true);
$coursename         = trim($body['coursename']         ?? '');
$total_credits      = (int)($body['total_credits']      ?? 0);
$pass_points        = (int)($body['pass_points']        ?? 0);
$merit_points       = (int)($body['merit_points']       ?? 0);
$distinction_points = (int)($body['distinction_points'] ?? 0);
$raw_qual_type      = trim($body['qual_type']           ?? '');

if (!$coursename) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Course name required']);
    exit();
}

try {
    $db = getDb();

    // Validate qual_type against the qualification type table
    $chk = $db->prepare('SELECT code FROM tblqualificationtype WHERE code = ?');
    $chk->execute([$raw_qual_type]);
    $qual_type = $chk->fetchColumn() ?: 'other';

    $stmt = $db->prepare(
        'INSERT INTO tblcourse
         (coursename, total_credits, pass_points, merit_points, distinction_points, qual_type)
         VALUES (?,?,?,?,?,?)'
    );
    $stmt->execute([$coursename, $total_credits, $pass_points, $merit_points, $distinction_points, $qual_type]);
    $id = $db->lastInsertId();

    http_response_code(201);
    echo json_encode(['success' => true, 'data' => ['id' => $id], 'message' => 'Course created']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
