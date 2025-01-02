<script setup lang="ts">
// TODO: Remove DeviceDetector for a .NET alternative
import DeviceDetector from "device-detector-js";
import { onMounted, ref, shallowRef, triggerRef } from "vue";
import { Checkbox, Panel } from "primevue";
import { Icon } from "@iconify/vue";
import type { Config, Preset } from "./types/config";
import CalendarButton from "./components/CalendarButton.vue";

const selectedCategories = ref<string[]>([]);
const getCustomConfig = (): Preset[] | null => {
  const urlParams = new URLSearchParams(window.location.search)
  const customConfig = urlParams.get('data');
  if (customConfig) return JSON.parse(atob(customConfig)) as Preset[];
  return null;
};

const deviceDetector = new DeviceDetector();
const device = deviceDetector.parse(navigator.userAgent);
const version = `${device.os?.name}${device.os?.version}/${device.client?.name}`;

const isDarkMode = window.matchMedia("(prefers-color-scheme: dark)").matches;

let config = shallowRef<Config>({
  presets: [],
  supportEmail: ""
})

onMounted(async () => {
  if (import.meta.env.PROD) {
    config.value = await fetch("/config").then((res) => res.json())
  } else {
    config.value = await import("./assets/config.json");
  }
  let customConfig = getCustomConfig();
  if (customConfig) {
    customConfig = customConfig.map((preset: any) => {
      preset.custom = true;
      return preset;
    });
    config.value = {
      ...config.value,
      presets: [...config.value.presets, ...customConfig]
    };
  }
  triggerRef(config)
});
</script>

<template>
  <div class="flex h-screen w-screen md:p-32 p-4">
    <div class="flex-1 flex flex-col justify-center items-center gap-4">
      <img :src="isDarkMode ? '/logo-dark.svg' : '/logo-light.svg'" class="h-16" />
      <h1 class="text-4xl font-medium text-center leading-tight">My Church Calendar</h1>
      <Panel class="text-xl my-3" header="AdSafe" v-if="config.presets.find(preset => preset.custom)">
        <Icon icon="charm:shield-tick" class="inline-block italic text-cyan-600 dark:text-cyan-500 text-2xl" />
        You have been shared a private calendar. Please do not share this link with others.
      </Panel>
      <div v-if="config.presets.length > 0" class="place-items-center flex flex-col gap-2">
        <div v-for="category of config.presets" :key="category.id" class="flex items-center gap-2">
          <Checkbox v-model="selectedCategories" :inputId="category.id" name="category" :value="category.id" />
          <label :for="category.id" class="text-xl">
            {{ category.name }}
            <span v-if="category.recommended" class="inline-block text-teal-600 dark:text-teal-500">
              <Icon icon="charm:circle-tick" class="inline-block italic mx-1" />Recommended
            </span>
            <span v-if="category.custom" class="inline-block text-cyan-600 dark:text-cyan-500">
              <Icon icon="charm:shield-tick" class="inline-block italic mx-1" />Private
            </span>
          </label>
        </div>
        <CalendarButton :disabled="!selectedCategories.length" :getIds="selectedCategories" :os="device.os?.name" />
      </div>
      <div v-else class="text-xl">
        No calendars have been made available yet.
      </div>
      <Panel header="Report an Error" class="w-full mt-2" toggleable :collapsed="true">
        <p class="mb-2">
          For technical help or to report an issue, please email <a :href="`mailto:${config.supportEmail}`">{{
            config.supportEmail }}</a>.
        </p>
        <span class="text-slate-500" style="font-family: monospace;">{{ version }}</span>
      </Panel>
    </div>
  </div>
</template>

