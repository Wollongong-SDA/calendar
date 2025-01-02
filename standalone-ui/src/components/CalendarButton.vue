<script setup lang="ts">
import { computed, defineProps, ref } from "vue";
import { Button, Dialog, InputText, SplitButton } from "primevue";
import type { MenuItem } from "primevue/menuitem";
const props = defineProps({
  disabled: Boolean,
  getIds: {
    type: Array,
    required: true
  },
  os: String
});

const androidDialog = ref(false);
const copyDialog = ref(false);
const copySuccessDialog = ref(false);

const getLink = computed((): string => {
  const sortedCategories = props.getIds;
  sortedCategories.sort();
  const link = `webcal://${document.location.host}/calendar.ics?id=${sortedCategories.join(",")}`
  console.debug(link);
  return link;
});

const subscribeButton = () => {
  if (props.os === "Android") {
    androidDialog.value = true;
    return;
  }
  subscribe();
};

const subscribe = () => {
  document.location.href = getLink.value;
};

const googleCalendar = () => {
  document.location.href = 'https://www.google.com/calendar/render?cid=' + encodeURIComponent(getLink.value);
};

const outlook = () => {
  // outlook.live.com redirects to homepage if not signed in, outlook.office.com prefers Work/School accounts but work with personal accounts too
  document.location.href = 'https://outlook.office.com/calendar/0/addfromweb/?url=' + encodeURIComponent(getLink.value);
};

const copyLink = () => {
  navigator.clipboard.writeText(getLink.value).then(() => copySuccessDialog.value = true).catch(() => copyDialog.value = true);
};

const buttons: MenuItem[] = [
  {
    label: "Add to Apple Calendar",
    command: subscribe,
    visible: props.os == "iOS" || props.os == "Mac"
  },
  {
    label: "Add to Google Calendar",
    command: googleCalendar
  },
  {
    label: "Add to Outlook",
    command: outlook
  },
  {
    label: "Copy Link",
    command: copyLink
  }
]
</script>

<template>
  <SplitButton :disabled="disabled" @click="subscribeButton" class="mt-4" label="Subscribe" :model="buttons" />

  <!-- Android -->
  <Dialog :visible="androidDialog" modal header="Android" :closable="false" :draggable="false"
    :style="{ width: '25rem' }">
    <div class="flex items-center gap-4 mb-4">
      Most Android devices do not support automatic calendar subscriptions. You can still add the calendar using Google,
      Outlook or other preferred calendars by copying the link.
    </div>
    <div class="flex justify-end gap-2">
      <Button label="Subscribe" severity="secondary" @click="androidDialog = false; subscribe()" />
      <Button label="Copy Link" @click="androidDialog = false; copyLink()" />
    </div>
  </Dialog>

  <!-- Copy Link -->
  <Dialog :visible="copyDialog" modal header="Copy Link" :closable="false" :draggable="false"
    :style="{ width: '25rem' }">
    <div class="flex flex-col items-center gap-4 mb-4">
      Paste the following link into your calendar's "Subscription" or "From URL / Web" feature.
      <InputText v-model="getLink" class="w-full" readonly />
    </div>
    <div class="flex justify-end gap-2">
      <Button label="Continue" @click="copyDialog = false" />
    </div>
  </Dialog>

  <!-- Copy Success -->
  <Dialog :visible="copySuccessDialog" modal header="Link Copied Successfully" :closable="false" :draggable="false"
    :style="{ width: '25rem' }">
    <div class="flex flex-col items-center gap-4 mb-4">
      Paste the link into your calendar's "Subscription" or "From URL / Web" feature.
    </div>
    <div class="flex justify-end gap-2">
      <Button label="Continue" @click="copySuccessDialog = false" />
    </div>
  </Dialog>
</template>
