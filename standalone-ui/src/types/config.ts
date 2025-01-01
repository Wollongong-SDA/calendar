export type Config = {
  presets: Preset[],
  supportEmail: string,
  coordinatorEmail: string,
}

export type Preset = {
  name: string,
  id: string,
  custom?: boolean,
  recommended?: boolean,
}