/**
 * Cryptographic utilities for OAuth 2.0 PKCE (Proof Key for Code Exchange)
 */

/**
 * Generate a cryptographically random code verifier for PKCE
 * @returns Base64-URL encoded random string (43 characters)
 */
export function generateCodeVerifier(): string {
  const array = new Uint8Array(32);
  crypto.getRandomValues(array);
  return base64UrlEncode(array);
}

/**
 * Generate a code challenge from a code verifier using SHA-256
 * @param verifier The code verifier to hash
 * @returns Base64-URL encoded SHA-256 hash
 */
export async function generateCodeChallenge(verifier: string): Promise<string> {
  const encoder = new TextEncoder();
  const data = encoder.encode(verifier);
  const hash = await crypto.subtle.digest('SHA-256', data);
  return base64UrlEncode(new Uint8Array(hash));
}

/**
 * Generate a random state parameter for CSRF protection
 * @returns Base64-URL encoded random string
 */
export function generateState(): string {
  const array = new Uint8Array(16);
  crypto.getRandomValues(array);
  return base64UrlEncode(array);
}

/**
 * Encode a byte array as Base64-URL (RFC 4648)
 * @param array Byte array to encode
 * @returns Base64-URL encoded string
 */
function base64UrlEncode(array: Uint8Array): string {
  return btoa(String.fromCharCode(...array))
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=/g, '');
}

/**
 * Decode a Base64-URL string to a regular string
 * @param str Base64-URL encoded string
 * @returns Decoded string
 */
export function base64UrlDecode(str: string): string {
  const base64 = str.replace(/-/g, '+').replace(/_/g, '/');
  const padding = '='.repeat((4 - (base64.length % 4)) % 4);
  return atob(base64 + padding);
}
