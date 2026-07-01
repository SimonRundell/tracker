<?php
/**
 * DELETE /qualtypes/delete.php
 *
 * Deletes a qualification type. Blocked if any courses currently use this
 * type; returns the count in the error message so the UI can inform the user.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

if ($_SERVER['REQUEST_METHOD'] !== 'DELETE') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$body = json_decode(file_get_contents('php://input'), true);
$id   = (int)($body['id'] ?? 0);

if (!$id) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'ID required']);
    exit();
}

try {
    $db = getDb();

    // Get the code first so we can check course usage
    $row = $db->prepare('SELECT code FROM tblqualificationtype WHERE id = ?');
    $row->execute([$id]);
    $qt = $row->fetch();

    if (!$qt) {
        http_response_code(404);
        echo json_encode(['success' => false, 'error' => 'Qualification type not found']);
        exit();
    }

    // Check how many courses use this type
    $chk  = $db->prepare('SELECT COUNT(*) FROM tblcourse WHERE qual_type = ?');
    $chk->execute([$qt['code']]);
    $used = (int)$chk->fetchColumn();

    if ($used > 0) {
        http_response_code(409);
        $noun = $used === 1 ? 'course uses' : 'courses use';
        echo json_encode([
            'success' => false,
            'error'   => "$used $noun this qualification type. Reassign or delete those courses first.",
            'count'   => $used,
        ]);
        exit();
    }

    $del = $db->prepare('DELETE FROM tblqualificationtype WHERE id = ?');
    $del->execute([$id]);

    echo json_encode(['success' => true, 'message' => 'Qualification type deleted']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
