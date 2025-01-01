export type Config = {
  presets: Preset[],
  supportEmail: string
}

export type Preset = {
  name: string,
  id: string,
  custom?: boolean,
  recommended?: boolean,
}