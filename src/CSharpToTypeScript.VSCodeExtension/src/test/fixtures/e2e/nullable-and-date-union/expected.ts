export interface AuditDto {
    processedAt?: string | Date;
    syncedAt?: string | Date;
    retryCount?: number;
    error?: string;
}
