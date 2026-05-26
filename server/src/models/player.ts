export interface PlayerDocument {
  id: string;
  save_version: number;
  profile_json: Record<string, unknown>;
  updated_at: string;
}

export interface SavePayload {
  saveVersion: number;
  profileJson: string;
}
