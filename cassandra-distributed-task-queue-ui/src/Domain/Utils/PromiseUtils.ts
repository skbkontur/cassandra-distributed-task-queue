export function delay(timeout: number): Promise<void> {
    return new Promise(f => setTimeout(f, timeout));
}
