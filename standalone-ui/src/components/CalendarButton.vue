<script setup lang="ts">
import { computed, ref } from "vue";
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

const androidDialog = ref(props.os == "Android");
const copyDialog = ref(false);
const copySuccessDialog = ref(false);

const getLink = computed((): string => {
  const sortedCategories = props.getIds;
  sortedCategories.sort();
  const link = `${document.location.host}/calendar.ics?id=${sortedCategories.join(",")}`
  console.debug(link);
  return link;
});

const subscribe = () => document.location.href = 'webcal://' + getLink.value;

const googleCalendar = () => document.location.href = 'https://calendar.google.com/calendar/render?cid=' + encodeURIComponent('webcal://' + getLink.value);

const outlook = () => document.location.href = 'https://outlook.office.com/calendar/0/addfromweb/?url=' + encodeURIComponent('webcal://' + getLink.value);

const copyLink = () => navigator.clipboard.writeText('webcal://' + getLink.value).then(() => copySuccessDialog.value = true).catch(() => copyDialog.value = true);

const buttons: MenuItem[] = [
  {
    label: "Add to Apple Calendar",
    command: subscribe,
    visible: props.os == "iOS" || props.os == "Mac"
  },
  {
    label: "Add to Google Calendar",
    command: googleCalendar,
    visible: props.os != "Android"
  },
  {
    label: "Add to Outlook",
    command: outlook,
    visible: props.os != "Android" && props.os != "iOS"
  },
  {
    label: "Copy Link",
    command: copyLink
  }
]
</script>

<template>
  <SplitButton :disabled="disabled" @click="subscribe" class="mt-4" label="Subscribe" :model="buttons" />

  <!-- Android -->
  <Dialog :visible="androidDialog" modal header="Android" :closable="false" :draggable="false"
    :style="{ width: '25rem' }">
    <div class="flex items-center mb-4">
      Android devices do not support automatic calendar subscriptions. We recommend using a desktop to add to Google /
      Outlook, or copying the link to manually subscribe.
    </div>
    <div class="flex justify-end">
      <Button label="Continue" severity="primary" @click="androidDialog = false" />
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
