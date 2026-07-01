<?php
/**
 * POST /students/import.php
 *
 * Bulk upsert of student records from a CSV upload, with optional
 * enrollment into a teaching group.
 *
 * Expects a JSON body:
 *   {
 *     "students": [{ "firstname": "", "lastname": "", "cisnumber": "" }, ...],
 *     "group_id": int|null
 *   }
 *
 * For each student:
 *   - If cisnumber is present and already in the DB, the name is updated.
 *   - Otherwise a new student record is inserted.
 *   - If group_id is supplied the student is enrolled via INSERT IGNORE
 *     (already-enrolled students are silently skipped).
 *
 * Response:
 *   { "success": true, "data": { "imported": N, "updated": N, "errors": [...] } }
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

$body     = json_decode(file_get_contents('php://input'), true);
$students = $body['students'] ?? [];
$groupId  = isset($body['group_id']) ? ((int)$body['group_id'] ?: null) : null;

if (!is_array($students) || count($students) === 0) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'No student rows supplied']);
    exit();
}

$db = getDb();

/**
 * Upsert by cisnumber; use LAST_INSERT_ID(id) trick so lastInsertId()
 * returns the existing PK on a duplicate-key update.
 */
$upsertSql = <<<SQL
    INSERT INTO tblstudent (firstname, lastname, cisnumber)
    VALUES (?, ?, ?)
    ON DUPLICATE KEY UPDATE
        firstname = VALUES(firstname),
        lastname  = VALUES(lastname),
        id        = LAST_INSERT_ID(id)
SQL;

$insertSql  = 'INSERT INTO tblstudent (firstname, lastname) VALUES (?, ?)';
$enrollSql  = 'INSERT IGNORE INTO tblstudent_group (student_id, group_id) VALUES (?, ?)';

$upsertStmt = $db->prepare($upsertSql);
$insertStmt = $db->prepare($insertSql);
$enrollStmt = $groupId ? $db->prepare($enrollSql) : null;

$imported = 0;
$updated  = 0;
$errors   = [];

foreach ($students as $i => $row) {
    $rowNum    = $i + 1;
    $firstname = trim($row['firstname'] ?? '');
    $lastname  = trim($row['lastname']  ?? '');
    $cisnumber = trim($row['cisnumber'] ?? '') ?: null;

    if ($firstname === '' || $lastname === '') {
        $errors[] = ['row' => $rowNum, 'message' => 'Missing firstname or lastname'];
        continue;
    }

    try {
        if ($cisnumber !== null) {
            $upsertStmt->execute([$firstname, $lastname, $cisnumber]);
            $studentId = (int)$db->lastInsertId();
            // rowCount: 1 = inserted, 2 = updated, 0 = unchanged
            $affected  = $upsertStmt->rowCount();
            if ($affected === 1) { $imported++; } else { $updated++; }
        } else {
            $insertStmt->execute([$firstname, $lastname]);
            $studentId = (int)$db->lastInsertId();
            $imported++;
        }

        if ($enrollStmt && $studentId) {
            $enrollStmt->execute([$studentId, $groupId]);
        }
    } catch (Exception $e) {
        $errors[] = ['row' => $rowNum, 'message' => 'Database error: ' . $e->getMessage()];
    }
}

echo json_encode([
    'success' => true,
    'data'    => ['imported' => $imported, 'updated' => $updated, 'errors' => $errors],
]);
