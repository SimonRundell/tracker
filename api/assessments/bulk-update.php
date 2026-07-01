<?php
/**
 * PUT /assessments/bulk-update.php
 *
 * Applies a set of assessment part values to every student in a group.
 * Only rows where the student already has the target status (or any status,
 * depending on the overwrite_mode flag) are affected.
 *
 * Request body:
 *   {
 *     "group_id": 3,
 *     "overwrite_mode": "not_set" | "all",
 *     "updates": [
 *       {
 *         "assessment_def_id": 5,
 *         "status": "SET",
 *         "date_set": "2026-06-01",
 *         "date_deadline": "2026-07-01",
 *         "date_resubmission": null,
 *         "date_completed": null
 *       },
 *       ...
 *     ]
 *   }
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

$auth = requireAuth();

if ($_SERVER['REQUEST_METHOD'] !== 'PUT') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$body           = json_decode(file_get_contents('php://input'), true);
$group_id       = (int)($body['group_id']       ?? 0);
$overwrite_mode = $body['overwrite_mode'] ?? 'not_set'; // 'not_set' | 'all'
$updates        = $body['updates'] ?? [];

if (!$group_id || !is_array($updates) || empty($updates)) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'group_id and updates required']);
    exit();
}

$valid_statuses = ['NOT_SET','SET','HANDED_IN_1','RETURNED','HANDED_IN_2','COMPLETE','INCOMPLETE'];

/** Returns a valid date string or NULL. */
function sanitiseDate(?string $val): ?string {
    if (!$val) return null;
    $d = \DateTime::createFromFormat('Y-m-d', $val);
    return ($d && $d->format('Y-m-d') === $val) ? $val : null;
}

try {
    $db      = getDb();
    $user_id = (int)($auth->sub ?? 0);

    // Fetch all student IDs in this group
    $sStmt = $db->prepare('SELECT id FROM tblstudent WHERE group_id = ?');
    $sStmt->execute([$group_id]);
    $studentIds = array_column($sStmt->fetchAll(), 'id');

    if (empty($studentIds)) {
        echo json_encode(['success' => true, 'affected' => 0]);
        exit();
    }

    $affected = 0;

    foreach ($updates as $row) {
        $def_id    = (int)($row['assessment_def_id'] ?? 0);
        $status    = in_array($row['status'] ?? '', $valid_statuses) ? $row['status'] : 'NOT_SET';
        $date_set  = sanitiseDate($row['date_set']          ?? null);
        $deadline  = sanitiseDate($row['date_deadline']     ?? null);
        $resub     = sanitiseDate($row['date_resubmission'] ?? null);
        $completed = sanitiseDate($row['date_completed']    ?? null);

        if (!$def_id) continue;

        if ($overwrite_mode === 'not_set') {
            // Upsert only where no row exists yet (INSERT IGNORE), then UPDATE only NOT_SET rows
            $insertSql = 'INSERT IGNORE INTO tblassessment
                          (student_id, assessment_def_id, status, date_set, date_deadline, date_resubmission, date_completed, set_by_id, updated_by)
                          SELECT id, ?, ?, ?, ?, ?, ?, ?, ?
                          FROM tblstudent
                          WHERE group_id = ?';
            $iStmt = $db->prepare($insertSql);
            $iStmt->execute([$def_id, $status, $date_set, $deadline, $resub, $completed, $user_id, $user_id, $group_id]);

            // Also update existing NOT_SET rows
            $updateSql = 'UPDATE tblassessment a
                          JOIN tblstudent s ON s.id = a.student_id
                          SET a.status = ?, a.date_set = ?, a.date_deadline = ?, a.date_resubmission = ?,
                              a.date_completed = ?,
                              a.set_by_id = CASE WHEN ? IS NOT NULL THEN ? ELSE a.set_by_id END,
                              a.updated_by = ?, a.updated_at = CURRENT_TIMESTAMP
                          WHERE s.group_id = ? AND a.assessment_def_id = ? AND a.status = \'NOT_SET\'';
            $uStmt = $db->prepare($updateSql);
            $uStmt->execute([$status, $date_set, $deadline, $resub, $completed,
                             $date_set, $user_id, $user_id, $group_id, $def_id]);
            $affected += $iStmt->rowCount() + $uStmt->rowCount();

        } else {
            // Overwrite all: upsert for every student
            $sql = 'INSERT INTO tblassessment
                      (student_id, assessment_def_id, status, date_set, date_deadline, date_resubmission, date_completed, set_by_id, updated_by)
                    SELECT id, ?, ?, ?, ?, ?, ?, ?, ?
                    FROM tblstudent
                    WHERE group_id = ?
                    ON DUPLICATE KEY UPDATE
                      status = VALUES(status),
                      date_set = VALUES(date_set),
                      date_deadline = VALUES(date_deadline),
                      date_resubmission = VALUES(date_resubmission),
                      date_completed = VALUES(date_completed),
                      set_by_id = CASE WHEN VALUES(date_set) IS NOT NULL THEN VALUES(set_by_id) ELSE set_by_id END,
                      updated_by = VALUES(updated_by),
                      updated_at = CURRENT_TIMESTAMP';
            $stmt = $db->prepare($sql);
            $stmt->execute([$def_id, $status, $date_set, $deadline, $resub, $completed, $user_id, $user_id, $group_id]);
            $affected += $stmt->rowCount();
        }
    }

    echo json_encode(['success' => true, 'affected' => $affected]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
